package com.ruoyi.framework.web.domain;

import com.ruoyi.common.constant.HttpStatus;
import com.ruoyi.common.utils.StringUtils;

import java.util.HashMap;
import java.util.Objects;

/**
 * 操作消息提醒
 *
 * @author ruoyi
 */
public class AjaxResultNew extends HashMap<String, Object>
{
    private static final long serialVersionUID = 1L;

    /** 状态码 */
    public static final String CODE_TAG = "status";

    /** 返回内容 */
    public static final String MSG_TAG = "msg";

    /** 返回内容编码 */
    public static final String MSG_CODE = "msg_code";

    /** 数据对象 */
    public static final String DATA_TAG = "data";

    /**
     * 初始化一个新创建的 AjaxResult 对象，使其表示一个空消息。
     */
    public AjaxResultNew()
    {
    }

    /**
     * 初始化一个新创建的 AjaxResult 对象
     *
     * @param bol 状态码
     * @param msg 返回内容
     */
    public AjaxResultNew(boolean bol, String msg, String msgCode)
    {
        super.put(CODE_TAG, bol);
        super.put(MSG_TAG, msg);
        super.put(MSG_CODE, msgCode);
    }

    /**
     * 初始化一个新创建的 AjaxResult 对象
     *
     * @param bol 状态码
     * @param msg 返回内容
     * @param data 数据对象
     */
    public AjaxResultNew(boolean bol, String msg, String msgCode ,Object data)
    {
        super.put(CODE_TAG, bol);
        super.put(MSG_TAG, msg);
        super.put(MSG_CODE, msgCode);
        if (StringUtils.isNotNull(data))
        {
            super.put(DATA_TAG, data);
        }
    }

    /**
     * 返回成功消息
     *
     * @return 成功消息
     */
    public static AjaxResultNew success()
    {
        return AjaxResultNew.success("操作成功");
    }

    /**
     * 返回成功数据
     *
     * @return 成功消息
     */
    public static AjaxResultNew success(Object data)
    {
        return AjaxResultNew.success("操作成功", data);
    }

    /**
     * 返回成功消息
     *
     * @param msg 返回内容
     * @return 成功消息
     */
    public static AjaxResultNew success(String msg)
    {
        return AjaxResultNew.success(msg, null);
    }

    /**
     * 返回成功消息
     *
     * @param msg 返回内容
     * @param data 数据对象
     * @return 成功消息
     */
    public static AjaxResultNew success(String msg, Object data)
    {
        return new AjaxResultNew(true, msg, "GetSucc",data);
    }

    /**
     * 返回警告消息
     *
     * @param msg 返回内容
     * @return 警告消息
     */
    public static AjaxResultNew warn(String msg)
    {
        return AjaxResultNew.warn(msg, null);
    }

    /**
     * 返回警告消息
     *
     * @param msg 返回内容
     * @param data 数据对象
     * @return 警告消息
     */
    public static AjaxResultNew warn(String msg, Object data)
    {
        return new AjaxResultNew(true, msg, "GetSucc" , data);
    }

    /**
     * 返回错误消息
     *
     * @return 错误消息
     */
    public static AjaxResultNew error()
    {
        return AjaxResultNew.error("操作失败");
    }

    /**
     * 返回错误消息
     *
     * @param msg 返回内容
     * @return 错误消息
     */
    public static AjaxResultNew error(String msg)
    {
        return AjaxResultNew.error(msg, null);
    }

    /**
     * 返回错误消息
     *
     * @param msg 返回内容
     * @param data 数据对象
     * @return 错误消息
     */
    public static AjaxResultNew error(String msg, Object data)
    {
        return new AjaxResultNew(false, msg,"GetErr" ,data);
    }

    /**
     * 返回错误消息
     *
     * @param bol 状态码
     * @param msg 返回内容
     * @return 错误消息
     */
    public static AjaxResultNew error(Boolean bol, String msg)
    {
        return new AjaxResultNew(bol, msg, null);
    }

    /**
     * 是否为成功消息
     *
     * @return 结果
     */
    public boolean isSuccess()
    {
        return Objects.equals(HttpStatus.SUCCESS, this.get(CODE_TAG));
    }

    /**
     * 是否为警告消息
     *
     * @return 结果
     */
    public boolean isWarn()
    {
        return Objects.equals(HttpStatus.WARN, this.get(CODE_TAG));
    }

    /**
     * 是否为错误消息
     *
     * @return 结果
     */
    public boolean isError()
    {
        return Objects.equals(HttpStatus.ERROR, this.get(CODE_TAG));
    }

    /**
     * 方便链式调用
     *
     * @param key 键
     * @param value 值
     * @return 数据对象
     */
    @Override
    public AjaxResultNew put(String key, Object value)
    {
        super.put(key, value);
        return this;
    }
}
